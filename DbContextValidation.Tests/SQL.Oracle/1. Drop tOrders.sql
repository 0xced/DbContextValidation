-- See https://stackoverflow.com/questions/1799128/oracle-if-table-exists/1801453#1801453
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE "tOrders"';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
      END IF;
END;