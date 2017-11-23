# qbtnet
Query by Text in .Net

This library heavily inspired by the OData makes it easier to write universal providers to data storages.
It transforms a text query into an expression tree consumable by the Entity Framework queries.

In tests you can find self disctiptive examples.

*It currently supports only one binary operation at a time.*

### Simple example:


#### Supported operators:
- [x] **eq** Equal
- [x] **ne** Not equal 
- [x] **gt** Greater than 
- [x] **lt** Less than 
- [x] **ge** Grater than or equal 
- [x] **ge** Less than or equal 
- [ ] **and** And 
- [ ] **or** Or 

#### Things to be added:
- [x] Support both for camel case and pascal case field names
- [ ] Bracket support
- [ ] Order by
- [ ] First
- [ ] Last
- [ ] string methods e.g. contains, endswith etc.
- [ ] Take
- [ ] Offset
