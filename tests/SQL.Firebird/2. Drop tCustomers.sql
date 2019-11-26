-- See http://www.firebirdfaq.org/faq69/
EXECUTE BLOCK AS BEGIN
IF (EXISTS(SELECT 1 FROM rdb$relations WHERE rdb$relation_name = 'tCustomers')) THEN EXECUTE STATEMENT 'DROP TABLE "tCustomers";';
END;
