define CREATE_TABLES
CREATE TABLE tCustomers (
  Id INTEGER NOT NULL,
  Name VARCHAR(50) NOT NULL,
  PRIMARY KEY (Id)
);

CREATE TABLE tOrders (
  Id INTEGER NOT NULL,
  OrderDate DATETIME NOT NULL,
  CustomerId INTEGER NOT NULL,
  PRIMARY KEY (Id),
  FOREIGN KEY(CustomerId) REFERENCES tCustomers(Id)
);
endef

export CREATE_TABLES

create_database:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db "$$CREATE_TABLES"
