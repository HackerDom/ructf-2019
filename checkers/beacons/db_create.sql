CREATE DATABASE checker;
\c checker;

-- for debug
DROP TABLE IF EXISTS workers CASCADE;
DROP TABLE IF EXISTS jobs CASCADE;
DROP TABLE IF EXISTS jobs_history CASCADE;
DROP TABLE IF EXISTS teams_state CASCADE;
DROP TABLE IF EXISTS teams_state_history CASCADE;
--DROP TABLE IF EXISTS beacon_coords CASCADE;    --do it manually and carefully

CREATE TYPE worker_state as ENUM ('free', 'busy', 'down');


CREATE TABLE IF NOT EXISTS workers
(id SERIAL PRIMARY KEY,
state state_enum NOT NULL DEFAULT 'free',
host VARCHAR(20) NOT NULL,
port INT NOT NULL,
UNIQUE (host, port));

CREATE TABLE IF NOT EXISTS jobs
(worker_id INT REFERENCES workers (id) UNIQUE,
team_ip VARCHAR(20) NOT NULL,
init_time TIME DEFAULT now(),
flag_id VARCHAR(50),
flag VARCHAR(32),
vuln INT NOT NULL,
UNIQUE (team_ip, vuln));

CREATE TABLE IF NOT EXISTS jobs_history
(worker_id INT,
team_ip VARCHAR(20),
init_time TIME,
flag_id VARCHAR(50),
flag VARCHAR(32),
vuln INT);

CREATE TABLE IF NOT EXISTS teams_state
(team_ip VARCHAR(20) NOT NULL,
vuln INT NOT NULL,
put INT CHECK (put in (101, 102, 103, 104, 110)) DEFAULT NULL,
get INT CHECK (get in (101, 102, 103, 104, 110)) DEFAULT NULL,
refresh_time TIME DEFAULT now(),
UNIQUE (team_ip, vuln));

CREATE TABLE IF NOT EXISTS teams_state_history
(team_ip VARCHAR(20),
vuln INT,
put INT CHECK (put in (101, 102, 103, 104, 110)),
get INT CHECK (get in (101, 102, 103, 104, 110)),
refresh_time TIME);


CREATE OR REPLACE FUNCTION update_teams_state()
RETURNS TRIGGER AS $$
BEGIN
    NEW.refresh_time = now();
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_teams_state_trigger
BEFORE UPDATE
ON teams_state
FOR EACH ROW EXECUTE PROCEDURE update_timestamp();

CREATE OR REPLACE FUNCTION make_worker_free()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE workers SET state='free' WHERE id=OLD.worker_id;
    RETURN OLD;
END;
$$ language 'plpgsql';

CREATE TRIGGER make_worker_free_trigger
BEFORE DELETE
ON jobs
FOR EACH ROW EXECUTE PROCEDURE make_worker_free(); 

CREATE OR REPLACE FUNCTION make_worker_busy()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE workers SET state='busy' WHERE id=NEW.worker_id;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER make_worker_busy_trigger
BEFORE INSERT
ON jobs
FOR EACH ROW EXECUTE PROCEDURE make_worker_busy();

CREATE OR REPLACE FUNCTION move_to_teams_history()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO teams_state_history (team_ip, vuln, put, get, refresh_time)
    VALUES (OLD.team_ip, OLD.vuln, OLD.put, OLD.get, OLD.refresh_time);
    RETURN OLD;
END;
$$ language 'plpgsql';

CREATE TRIGGER save_state
AFTER UPDATE
ON teams_state
FOR EACH ROW EXECUTE PROCEDURE move_to_teams_history();

CREATE OR REPLACE FUNCTION move_to_jobs_history()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO jobs_history (worker_id, team_ip, init_time, flag_id, flag, vuln)
    VALUES (OLD.worker_id, OLD.team_ip, OLD.init_time, OLD.flag_id, OLD.flag, OLD.vuln);
    RETURN OLD;
END;
$$ language 'plpgsql';

CREATE TRIGGER save_job
AFTER DELETE
ON jobs
FOR EACH ROW EXECUTE PROCEDURE move_to_jobs_history();
