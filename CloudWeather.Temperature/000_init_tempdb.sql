CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221020130328_initial-migration') THEN
    CREATE TABLE temperature (
        "Id" uuid NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "TempHighF" numeric NOT NULL,
        "TempLowF" numeric NOT NULL,
        "ZipCode" text NOT NULL,
        CONSTRAINT "PK_temperature" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221020130328_initial-migration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221020130328_initial-migration', '6.0.10');
    END IF;
END $EF$;
COMMIT;

