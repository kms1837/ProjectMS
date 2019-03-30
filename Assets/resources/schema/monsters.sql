create table monsters (
    id int NOT NULL Primary Key AUTOINCREMENT,
    name VARCHAR, 
    description VARCHAR,
    level INT, 
    hp FLOAT,
    mp FLOAT,
    dp FLOAT,
    defore_delay FLOAT,
    after_delay FLOAT,
    range FLOAT,
    power FLOAT,
    skill1 INT,
    skill2 INT,
    skill3 INT,
    skill4 INT
);
