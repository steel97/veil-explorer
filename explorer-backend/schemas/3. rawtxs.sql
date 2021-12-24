CREATE TABLE public.rawtxs (
	txid bytea NOT NULL,
	"data" bytea NOT NULL,
	CONSTRAINT rawtxs_pk PRIMARY KEY (txid),
	CONSTRAINT rawtxs_fk FOREIGN KEY (txid) REFERENCES public.transactions(txid) ON DELETE CASCADE
);