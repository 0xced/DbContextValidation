CREATE TABLE tOrders (
  Id INT IDENTITY(1, 1) PRIMARY KEY,
  OrderDate TIMESTAMP NOT NULL,
  CustomerId INTEGER NOT NULL,
  FOREIGN KEY (CustomerId) REFERENCES tCustomers(Id)
);
