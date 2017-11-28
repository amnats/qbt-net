# Query by Text in .Net

In a yak shaving accident I implemented a stripped down version of the OData provider. One can even say that it was a good exercise in understanding C# expression trees and LINQ queries.
All that this library can do is transform a text query into an expression tree consumable by any LINQ provider.

It can recognise field names and even discern strings from numbers!

#### Example

```cs
var expressionBuilder { get; } = new FilterExpressionBuilder<User>();

var filterStr = "userName eq 'Luke Skywalker'";
var expr = expressionBuilder.TranslateToExpression(filterStr);
var filtered = Users.Where(expr).ToList();

Assert.That(filtered.Count, Is.EqualTo(1);
Assert.That(filtered.First.UserName, Is.EqualTo("Luke Skywalker");
```

```cs
public class User
{
  public string UserName { get; set; }
}
```

#### Supported binary operators:
- [x] **eq** Equal
- [x] **ne** Not equal 
- [x] **gt** Greater than 
- [x] **lt** Less than 
- [x] **ge** Grater than or equal 
- [x] **le** Less than or equal 
- [ ] **and** And 
- [ ] **or** Or 

#### Nice things to have in future:
- [x] Support both for camel case and pascal case field names in the text query
- [ ] Support for more than one operator at a time
- [ ] Bracket support
- [ ] Order by
- [ ] First
- [ ] Last
- [ ] string methods e.g. contains, endswith etc.
- [ ] Take
- [ ] Offset
- [ ] Escaping characters
