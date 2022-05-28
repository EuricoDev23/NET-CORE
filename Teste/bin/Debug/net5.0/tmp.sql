CREATE TABLE Categoria (
IDCategoria int AUTO_INCREMENT NOT NULL,
Nome varchar(50) NOT NULL,
DateCreated DATETIME NOT NULL,
Status int NOT NULL,
PRIMARY KEY (IDCategoria)
);

CREATE TABLE Produto (
ProductID BIGINT AUTO_INCREMENT NOT NULL,
Nome varchar(100) NOT NULL,
IDCategoria int NULL,
IsStock bit NOT NULL,
DateCreated DATETIME NOT NULL,
Status int NOT NULL,
PRIMARY KEY (ProductID)
);

