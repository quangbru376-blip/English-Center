# Student registration feedback — 15/10/2026
The detail previously listed all unavailable action reasons, including hidden steps.
StudentActionReasons receives the actions rendered in its own button group.
It keeps server text, filters enabled/unrendered actions and groups identical reasons.
Booking reasons appear only beside an actual current booking.
No empty attempt grid is mounted before the first test booking.
Command handlers, server capabilities, payloads and session behavior are unchanged.
Three regression tests cover applicability, deduplication, escaping and no empty list.
186frontend tests/types/lint/build and mock Edge375/768/1280 passed.
The real owner's registration186 was not mutated by verification.
