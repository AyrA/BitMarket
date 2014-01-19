/*
Run this script with a root account.
It will create the Database and the user for it.



Create user for various localhost addresses
I recommend deleting the two statements you do not require.
Also change the Password!
*/

CREATE USER 'BitMarket'@'127.0.0.1' IDENTIFIED BY 'CHANGE-ME';
GRANT USAGE ON *.* TO 'BitMarket'@'127.0.0.1';
GRANT SELECT, EXECUTE, SHOW VIEW, DELETE, INSERT, UPDATE  ON `bitmarket`.* TO 'BitMarket'@'127.0.0.1';

CREATE USER 'BitMarket'@'localhost' IDENTIFIED BY 'CHANGE-ME';
GRANT USAGE ON *.* TO 'BitMarket'@'localhost';
GRANT SELECT, EXECUTE, SHOW VIEW, DELETE, INSERT, UPDATE  ON `bitmarket`.* TO 'BitMarket'@'localhost';

CREATE USER 'BitMarket'@'::1' IDENTIFIED BY 'CHANGE-ME';
GRANT USAGE ON *.* TO 'BitMarket'@'::1';
GRANT SELECT, EXECUTE, SHOW VIEW, DELETE, INSERT, UPDATE  ON `bitmarket`.* TO 'BitMarket'@'::1';

FLUSH PRIVILEGES;


/*Create Database*/
CREATE DATABASE `BitMarket`;
USE `BitMarket`;


/*Create Tables*/
CREATE TABLE `BitCategory` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) DEFAULT NULL,
  `Parent` int(11) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Contains categories';

CREATE TABLE `BitFile` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) DEFAULT NULL,
  `Address` varchar(40) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Contains file names and data types';

CREATE TABLE `BitOffer` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Title` varchar(50) DEFAULT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `Address` varchar(40) DEFAULT NULL,
  `Category` int(11) DEFAULT NULL,
  `Files` varchar(50) DEFAULT NULL,
  `Stock` int(11) DEFAULT NULL,
  `PriceMap` varchar(200) DEFAULT NULL,
  `LastModify` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Contains offers from various sellers';

CREATE TABLE `bittransaction` (
	`ID` INT(11) NOT NULL,
	`AddressBuyer` VARCHAR(40) NULL DEFAULT NULL,
	`AddressSeller` VARCHAR(40) NULL DEFAULT NULL,
	`Amount` INT(11) NULL DEFAULT NULL,
	`Offer` INT(11) NULL DEFAULT NULL,
	`State` INT(11) NULL DEFAULT NULL COMMENT '-3 RejectedByBoth; -2 RejectedByBuyer; -1 RejectedBySeller; 0 Neutral; 1 Confirmed; 2 Completed; 3 Commented; 4 NoBuyerComment; 5 NoSellerComment; 6 NoComment',
	`BuyerComment` VARCHAR(500) NULL DEFAULT NULL,
	`SellerComment` VARCHAR(500) NULL DEFAULT NULL,
	`BuyerRating` INT(11) NULL DEFAULT NULL,
	`SellerRating` INT(11) NULL DEFAULT NULL,
	`TransactionTime` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Table with transaction data. Used also as reference for ratings';

/*
Now that you have set up the database, create a DSN with the name "BitMarket"

- Under windows, run odbcad32.exe (inside system32 directory) and create a new User-DSN
  (or System-DSN, if you need it in another user account)
- Select the MySQL ODBC driver (if it is missing, get it: http://dev.mysql.com/downloads/connector/odbc/)
- Enter all fields:
  Data Source Name: BitMarket
  Description: <whatever you want>
  TCP/IP Server: <your MySQL server IP (Default: 127.0.0.1)>
  Port: <your MySQL server Port (Default: 3306)>
  User: BitMarket
  Password: <The password you entered in the file above>
  Database: BitMarket
*/
