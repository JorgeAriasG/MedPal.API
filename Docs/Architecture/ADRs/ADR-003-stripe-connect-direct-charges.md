# ADR-003: Stripe Connect direct charges for patient payments

Status: Proposed - legal and commercial validation required

Use connected accounts at the approved legal merchant boundary and create patient payment intents in connected-account scope. Keep ClinicFlow SaaS subscriptions on the platform Billing account. Consume distinct platform and Connect webhook streams and persist the connected account with every provider reference. Merchant-of-record, fees, disputes, refunds, tax, and negative-balance responsibility require human approval.

