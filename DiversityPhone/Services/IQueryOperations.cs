using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DiversityPhone.Services
{
    interface IQueryOperations<T>
    {
        Expression<Func<T, bool>> KeySmallerThanExpression(T item);
        Expression<Func<T, bool>> KeyEqualsExpression(T item);

    }
}
