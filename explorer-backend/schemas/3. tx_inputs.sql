CREATE TABLE public.tx_inputs (
	id uuid NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
	txid bytea NOT NULL,
	input_index integer NOT NULL,
	"type" smallint NOT NULL,
	num_inputs integer NULL,
	ring_size integer NULL,
	prev_txid bytea NULL,
	denomination text NULL,
	serial bytea NULL,
	pubcoin bytea NULL,
	vout bigint NULL,
	scriptsig_asm text NULL,
	scriptsig_hex bytea NULL,
	txinwitness bytea[] NULL,
	"sequence" bigint NOT NULL,
	CONSTRAINT tx_inputs_fk FOREIGN KEY (txid) REFERENCES public.transactions(txid)
);

CREATE INDEX tx_inputs_txid_idx ON public.tx_inputs (txid);