CREATE TABLE public.transactions (
	txid bytea NOT NULL,
	hash bytea NOT NULL,
	"version" integer NOT NULL,
	"size" integer NOT NULL,
	vsize integer NOT NULL,
	weight integer NOT NULL,
	locktime bigint NOT NULL,
	block_height integer NULL,
	CONSTRAINT transactions_pk PRIMARY KEY (txid),
	CONSTRAINT transactions_fk FOREIGN KEY (block_height) REFERENCES public.blocks(height) ON DELETE SET NULL
);
CREATE INDEX transactions_block_height_idx ON public.transactions (block_height);