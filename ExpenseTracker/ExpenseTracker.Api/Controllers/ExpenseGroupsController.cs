﻿using System;
using System.Linq;
using System.Web.Http;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Entities;
using ExpenseTracker.Repository.Factories;
using Marvin.JsonPatch;

namespace ExpenseTracker.API.Controllers
{
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

        public IHttpActionResult Get()
        {
            try
            {
                var expenseGroups = _repository.GetExpenseGroups();

                return Ok(expenseGroups.ToList()
                    .Select(eg => _expenseGroupFactory.CreateExpenseGroup(eg)));
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
    }
}
