# Label Taxonomy

## Status

- `status:planned`
- `status:ready`
- `status:in-progress`
- `status:review`
- `status:human-test`
- `status:blocked`

## Area

- `area:client`
- `area:server`
- `area:protocol`
- `area:persistence`
- `area:networking`
- `area:terrain`
- `area:infrastructure`
- `area:testing`

## Risk

- `risk:low`
- `risk:medium`
- `risk:high`

## Agent Routing

- `agent:small`
- `agent:standard`
- `agent:strong`

## Label Rules

- Apply exactly one status label.
- Apply all relevant area labels, but keep one area primary in the issue objective.
- Apply one risk label.
- Apply one agent-routing label before delegation.
- `status:ready` means all dependencies are merged and the issue passed specification review.
- `risk:high` work should normally use `agent:strong` and require explicit concurrency, recovery, compatibility, or security review.
