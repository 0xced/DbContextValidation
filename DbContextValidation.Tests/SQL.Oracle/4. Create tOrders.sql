CREATE TABLE "tOrders" (
  "Id" NUMBER PRIMARY KEY NOT NULL,
  "OrderDate" TIMESTAMP NOT NULL,
  "CustomerId" NUMBER NOT NULL,
  FOREIGN KEY("CustomerId") REFERENCES "tCustomers"("Id")
)