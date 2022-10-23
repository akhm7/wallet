# Task:

Implement Rest API for a financial institution where it provides e-wallet services to its partners. It has two types of e-wallet accounts: identified and unidentified.

The API can support multiple clients and should only use http, post methods with json as the data format. Clients must be authenticated via the http parameter header X-UserId and X-Digest.

X-Digest is hmac-sha1, the hash sum of the request body. There must be pre-recorded e-wallets, with different balances, and the maximum balance is 10,000 somoni for unidentified accounts and 100,000 somoni for identified accounts. You can use for data storage of your choice.

API service methods:

1. Check if the e-wallet account exists.
2. Replenish e-wallet account.
3. Get the total number and amount of recharge operations for the current month.
4. Get the e-wallet balance.

During development, use git and Github and make meaningful commits. Task results should be posted to your Github account, send us the link only. We do not accept task results in .zip / .rar, etc.
