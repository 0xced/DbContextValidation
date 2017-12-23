define CREATE_TABLES
CREATE TABLE Customers (
  Id INTEGER NOT NULL,
  Name VARCHAR(50) NOT NULL,
  PRIMARY KEY (Id)
);

CREATE TABLE Orders (
  Id INTEGER NOT NULL,
  OrderDate DATETIME NOT NULL,
  CustomerId INTEGER NOT NULL,
  PRIMARY KEY (Id),
  FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
);
endef

export CREATE_TABLES

create_database:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db "$$CREATE_TABLES"
