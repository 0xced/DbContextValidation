CREATE TABLE "tOrders" (
  "Id" SERIAL PRIMARY KEY,
  "OrderDate" TIMESTAMP NOT NULL,
  "CustomerId" INTEGER NOT NULL,
  FOREIGN KEY("CustomerId") REFERENCES "tCustomers"("Id")
);
