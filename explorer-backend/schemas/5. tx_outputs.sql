CREATE TABLE public.tx_outputs (
	id uuid NOT NULL DEFAULT gen_random_uuid(),
	txid bytea NOT NULL,
	output_index integer NOT NULL,
	"type" smallint NOT NULL DEFAULT 0,
	valuesat bigint NULL,
	vout_n integer NULL,
	scriptpub_asm text NULL,
	scriptpub_hex bytea NULL,
	scriptpub_type smallint NULL,
	reqsigs integer NULL,
	addresses text[] NULL,
	CONSTRAINT tx_outputs_pk PRIMARY KEY (id),
	CONSTRAINT tx_outputs_fk FOREIGN KEY (txid) REFERENCES public.transactions(txid)
);
CREATE INDEX tx_outputs_txid_idx ON public.tx_outputs (txid,output_index);

ALTER TABLE public.tx_outputs ADD data_hex bytea NULL;
ALTER TABLE public.tx_outputs ADD ct_fee text NULL;
ALTER TABLE public.tx_outputs ADD valuecommitment bytea NULL;
ALTER TABLE public.tx_outputs ADD pubkey bytea NULL;