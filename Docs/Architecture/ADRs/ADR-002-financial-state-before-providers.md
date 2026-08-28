# ADR-002: Internal financial state precedes provider integrations

Status: Proposed

Receivables, payments, allocations, refunds, and fiscal documents are internal domain records. Stripe and CFDI vendors are adapters. New monetary values use integer minor units plus ISO currency; transitions are audited and concurrency-protected. This supports cash, transfer, card, partial payment, refund, and future providers without provider-shaped persistence.

