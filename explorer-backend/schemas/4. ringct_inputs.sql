CREATE TABLE public.ringct_inputs (
	id uuid NOT NULL DEFAULT gen_random_uuid(),
	tx_input_id uuid NOT NULL,
	txid bytea NOT NULL,
	vout_n bigint NOT NULL,
	CONSTRAINT ringct_inputs_pk PRIMARY KEY (id),
	CONSTRAINT ringct_inputs_fk FOREIGN KEY (tx_input_id) REFERENCES public.tx_inputs(id) ON DELETE CASCADE
);
CREATE INDEX ringct_inputs_tx_input_id_idx ON public.ringct_inputs (tx_input_id);
CREATE INDEX ringct_inputs_txid_idx ON public.ringct_inputs (txid);