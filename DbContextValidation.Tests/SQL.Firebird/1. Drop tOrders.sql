-- See http://www.firebirdfaq.org/faq69/
EXECUTE BLOCK AS BEGIN
IF (EXISTS(SELECT 1 FROM rdb$relations WHERE rdb$relation_name = 'tOrders')) THEN EXECUTE STATEMENT 'DROP TABLE "tOrders";';
END;