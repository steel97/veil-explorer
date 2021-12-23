CREATE TABLE public.blocks (
	height integer NOT NULL,
	hash bytea NOT NULL,
	strippedsize integer NOT NULL DEFAULT 0,
	"size" integer NOT NULL DEFAULT 0,
	weight integer NOT NULL DEFAULT 0,
	proof_type smallint NOT NULL DEFAULT 0,
	proofofstakehash bytea NULL,
	progproofofworkhash bytea NULL,
	progpowmixhash bytea NULL,
	randomxproofofworkhash bytea NULL,
	sha256dproofofworkhash bytea NULL,
	proofofworkhash bytea NULL,
	"version" integer NOT NULL,
	merkleroot bytea NULL,
	"time" bigint NOT NULL,
	mediantime bigint NOT NULL,
	nonce bigint NOT NULL,
	nonce64 bigint NOT NULL,
	mixhash bytea NULL,
	bits bytea NOT NULL,
	difficulty bytea NOT NULL,
	chainwork bytea NULL,
	anon_index bigint NULL,
	veil_data_hash bytea NULL,
	prog_header_hash bytea NULL,
	prog_header_hex bytea NULL,
	epoch_number integer NULL,
	synced bool NOT NULL DEFAULT false,
	CONSTRAINT blocks_pk PRIMARY KEY (height)
);
CREATE INDEX blocks_hash_idx ON public.blocks (hash);
CREATE INDEX blocks_time_idx ON public.blocks ("time");
CREATE INDEX blocks_mediantime_idx ON public.blocks (mediantime);
CREATE INDEX blocks_height_idx ON public.blocks (height);