CREATE TABLE "tCustomers" (
  "Id" SERIAL PRIMARY KEY,
  "Name" VARCHAR(50) NOT NULL
);

CREATE TABLE "tOrders" (
  "Id" SERIAL PRIMARY KEY,
  "OrderDate" TIMESTAMP NOT NULL,
  "CustomerId" INTEGER NOT NULL,
  FOREIGN KEY("CustomerId") REFERENCES "tCustomers"("Id")
);
