CREATE TABLE track (
    id      INTEGER PRIMARY KEY,
    path    VARCHAR(255) UNIQUE NOT NULL,
    title   VARCHAR(30),
    artist  VARCHAR(30),
    year    INTEGER,
    comment VARCHAR(255)
);

CREATE TABLE album (
    id INTEGER PRIMARY KEY,
    name VARCHAR(30) UNIQUE NOT NULL
);

CREATE TABLE album_track (
    id_track INTEGER NOT NULL,
    id_album INTEGER NOT NULL,
    track_number INTEGER,
    UNIQUE (id_track, id_album)
);