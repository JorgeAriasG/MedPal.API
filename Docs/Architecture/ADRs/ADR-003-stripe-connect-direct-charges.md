# ADR-003: Stripe Connect direct charges for patient payments

Status: Accepted - merchant-of-record, fees, disputes, refunds, tax, and negative-balance responsibility still require external legal/commercial confirmation before T10 go-live

Use connected accounts at the approved legal merchant boundary and create patient payment intents in connected-account scope. Keep ClinicFlow SaaS subscriptions on the platform Billing account. Consume distinct platform and Connect webhook streams and persist the connected account with every provider reference. Merchant-of-record, fees, disputes, refunds, tax, and negative-balance responsibility require human approval.

Approved decisions: per-account (clinic) merchants; patient intents in connected scope; clinic staff owns Connect onboarding; patient portal offers guest checkout in v1. See `../Work-Packages/T02-decisions.md` (A3).

