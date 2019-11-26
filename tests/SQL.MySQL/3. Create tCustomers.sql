CREATE TABLE tCustomers (
  Id SERIAL PRIMARY KEY,
  Name VARCHAR(50) NOT NULL,
  AddressLine1 VARCHAR(100) NOT NULL,
  AddressLine2 VARCHAR(200) NOT NULL,
  AddressLine3 TEXT NOT NULL -- 65,535 bytes, see https://stackoverflow.com/questions/13932750/tinytext-text-mediumtext-and-longtext-maximum-storage-sizes/13932834#13932834
);
