-- Function: update_version(integer, integer)

-- DROP FUNCTION update_version(integer, integer);

CREATE OR REPLACE FUNCTION update_version("from" integer, "to" integer)
  RETURNS integer AS
$BODY$
DECLARE
    "current" integer;
BEGIN
    SELECT "version" into "current" from "version";
    IF "current" != "from" THEN
	RAISE EXCEPTION 'Current schema version is % (should be %)', "current", "from";  
    END IF;

    UPDATE "version" SET "version" = "to";
    RETURN "to";
END;
$BODY$
  LANGUAGE 'plpgsql' VOLATILE
  COST 100;
ALTER FUNCTION update_version(integer, integer) OWNER TO postgres;
GRANT EXECUTE ON FUNCTION update_version(integer, integer) TO public;
GRANT EXECUTE ON FUNCTION update_version(integer, integer) TO postgres;
COMMENT ON FUNCTION update_version(integer, integer) IS 'Update database schema version, check for current version, throw error if current version is not the expected version.';


-- Table: "version"

-- DROP TABLE "version";

CREATE TABLE "version"
(
  "version" integer NOT NULL,
  CONSTRAINT version_pkey PRIMARY KEY (version)
)
WITH (OIDS=FALSE);
ALTER TABLE "version" OWNER TO postgres;
GRANT SELECT, UPDATE ON TABLE "version" TO public;

