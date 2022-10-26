CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221020104331_initial-migration') THEN
    CREATE TABLE precipitation (
        "Id" uuid NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "AmountInches" numeric NULL,
        "WeatherType" text NOT NULL,
        "ZipCode" text NOT NULL,
        CONSTRAINT "PK_precipitation" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221020104331_initial-migration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221020104331_initial-migration', '6.0.10');
    END IF;
END $EF$;
COMMIT;

