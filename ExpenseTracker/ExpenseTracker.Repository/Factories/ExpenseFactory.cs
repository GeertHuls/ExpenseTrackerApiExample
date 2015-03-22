using ExpenseTracker.DTO;

namespace ExpenseTracker.Repository.Factories
{
    public class ExpenseFactory
    {

        public ExpenseFactory()
        {

        }

        public Expense CreateExpense(Entities.Expense expense)
        {
            return new Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }

        public Entities.Expense CreateExpense(Expense expense)
        {
            return new Entities.Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }
         
    }
}
