using ExpenseTracker.API.Helpers;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Entities;
using ExpenseTracker.Repository.Factories;
using Marvin.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;

namespace ExpenseTracker.API.Controllers
{
    [EnableCors("*", "*"/*ACCEPT*/, "GET,POST")]
    public class ExpenseGroupsController : ApiController
    {
        readonly IExpenseTrackerRepository _repository;
        readonly ExpenseGroupFactory _expenseGroupFactory = new ExpenseGroupFactory();

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new 
                ExpenseTrackerContext());
        }

        public ExpenseGroupsController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }

        const int MaxPageSize = 10;

        /// <summary>
        /// Examples:
        /// 
        /// http://localhost:679/api/expensegroups?sort=title
        /// http://localhost:679/api/expensegroups?sort=expenseGroupStatusId,title
        /// http://localhost:679/api/expensegroups?sort=-title (sort descending)
        /// 
        /// Filtering:
        /// http://localhost:679/api/expensegroups?status=open
        /// 
        /// Paging:
        /// http://localhost:679/api/expensegroups?page=2
        /// http://localhost:679/api/expensegroups?page=2&pageSize=2
        /// 
        /// DataShaping:
        /// http://localhost:679/api/expensegroups?fields=id,title
        /// http://localhost:679/api/expensegroups?fields=id,title,expenses //using association
        /// http://localhost:679/api/expensegroups?fields=id,title,expenses.id //only access id field of associated resource
        /// 
        /// </summary>
        [Route("api/expensegroups", Name = "ExpenseGroupsList")]
        public IHttpActionResult Get(string sort = "id",
            string status = null, string userId = null,
            string fields = null, 
            int page = 1, int pageSize = 5)
        {
            try
            {
                bool includeExpenses = false;
                List<string> lstOfFields = new List<string>();

                // we should include expenses when the fields-string contains "expenses", or "expenses.id", …
                if (fields != null)
                {
                    lstOfFields = fields.ToLower().Split(',').ToList();
                    includeExpenses = lstOfFields.Any(f => f.Contains("expenses"));
                }


                int statusId = -1;
                if (status != null)
                {
                    switch (status.ToLower())
                    {
                        case "open": statusId = 1;
                            break;
                        case "confirmed": statusId = 2;
                            break;
                        case "processed": statusId = 3;
                            break;
                        default:
                            break;
                    }
                }


                IQueryable<Repository.Entities.ExpenseGroup> expenseGroups = null;
                if (includeExpenses)
                {
                    expenseGroups = _repository.GetExpenseGroupsWithExpenses();
                }
                else
                {
                    expenseGroups = _repository.GetExpenseGroups();
                }

                expenseGroups = expenseGroups.ApplySort(sort)
                    .Where(eg => statusId == -1 || eg.ExpenseGroupStatusId == statusId)
                    .Where(eg => userId == null || eg.UserId == userId);



                // ensure the page size isn't larger than the maximum.
                if (pageSize > MaxPageSize)
                {
                    pageSize = MaxPageSize;
                }

                var totalCount = expenseGroups.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var urlHelper = new UrlHelper(Request);
                var prevLink = page > 1 ? urlHelper.Link("ExpenseGroupsList",
                    new
                    {
                        page = page - 1,
                        pageSize = pageSize,
                        sort = sort,
                        fields = fields,
                        status = status,
                        userId = userId
                    }) : "";
                var nextLink = page < totalPages ? urlHelper.Link("ExpenseGroupsList",
                    new
                    {
                        page = page + 1,
                        pageSize = pageSize,
                        sort = sort,
                        fields = fields,
                        status = status,
                        userId = userId
                    }) : "";

                var paginationHeader = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    previousPageLink = prevLink,
                    nextPageLink = nextLink
                };

                //Include in repsonse header:
                HttpContext.Current.Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));

                return Ok(expenseGroups
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToList()
                    .Select(eg => _expenseGroupFactory.CreateDataShapedObject(eg, lstOfFields)));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        public IHttpActionResult Get(int id)
        {
            try
            {
                var expenseGroup = _repository.GetExpenseGroup(id);

                return expenseGroup == null
                    ? (IHttpActionResult) NotFound()
                    : Ok(_expenseGroupFactory.CreateExpenseGroup(expenseGroup));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Test http post:
        /// 
        /// HEADER
        /// ------
        /// User-Agent: Fiddler
        /// Accept: application/json
        /// Host: localhost:53204
        /// Content-Length: 171
        /// Content-Type: application/json
        /// 
        /// CONTENT
        /// -------
        /// {
        ///  "userid" : "https://expsensetrackeridsrv3/embedded_1",
        ///  "title" : "New ExpenseGroup",
        ///  "description" : "ExpenseGroup description",
        ///  "expenseGroupStatusId" : 1
        /// }
        /// 
        /// </summary>
        [HttpPost]
        public IHttpActionResult Post([FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var group = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.InsertExpenseGroup(group);

                if (result.Status != RepositoryActionStatus.Created) return BadRequest();

                var newExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                    
                var uriNewExpenseGroup = string.Format("{0}/{1}", Request.RequestUri, newExpenseGroup.Id);
                return Created(uriNewExpenseGroup, newExpenseGroup);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Test http put:
        /// 
        /// HEADER
        /// ------
        /// User-Agent: Fiddler
        /// Accept: application/json
        /// Host: localhost:53204
        /// Content-Length: 171
        /// Content-Type: application/json
        /// 
        /// CONTENT
        /// -------
        /// {
        ///  "id" : 15,
        ///  "userid" : "https://expsensetrackeridsrv3/embedded_1",
        ///  "title" : "New ExpenseGroup updated",
        ///  "description" : "ExpenseGroup description",
        ///  "expenseGroupStatusId" : 1
        /// }
        /// 
        /// </summary>
        public IHttpActionResult Put(int id, [FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var group = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.UpdateExpenseGroup(group);

                switch (result.Status)
                {
                    case RepositoryActionStatus.Updated:
                        var updatedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                        return Ok(updatedExpenseGroup);
                    case RepositoryActionStatus.NotFound:
                        return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Test PATCH http://localhost:679/api/expensegroups/14
        /// 
        /// HEADER
        /// ------
        /// User-Agent: Fiddler
        /// Content-Type: application/json-patch+json
        /// 
        /// CONTENT
        /// -------
        /// [ { "op": "replace", "path": "/title", "value":"New Title Patched" },
        ///   { "op": "copy", "from": "/title", "path": "/description" } ]
        /// 
        /// </summary>
        [HttpPatch]
        public IHttpActionResult Patch(int id,
            [FromBody]JsonPatchDocument<DTO.ExpenseGroup> expenseGroupPatchDocument)
        {
            try
            {
                if (expenseGroupPatchDocument == null)
                {
                    return BadRequest();
                }

                var expenseGroup = _repository.GetExpenseGroup(id);
                if (expenseGroup == null)
                {
                    return NotFound();
                }

                // map
                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);

                // apply changes to the DTO
                expenseGroupPatchDocument.ApplyTo(eg);

                // map the DTO with applied changes to the entity, & update
                var result = _repository.UpdateExpenseGroup(_expenseGroupFactory.CreateExpenseGroup(eg));

                if (result.Status == RepositoryActionStatus.Updated)
                {
                    // map to dto
                    var patchedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                    return Ok(patchedExpenseGroup);
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Test DELETE http://localhost:679/api/expensegroups/14
        /// 
        /// HEADER
        /// ------
        /// User-Agent: Fiddler
        /// Content-Type: application/json
        /// 
        /// </summary>
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var result = _repository.DeleteExpenseGroup(id);

                if (result.Status == RepositoryActionStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
