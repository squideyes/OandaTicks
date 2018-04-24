To use the WinService you'll need an OANDA trading account.  You can signup for free at:

https://www.oanda.com/register/#/sign-up/demo

Once you have your credentials, create an AuthInfo.json file, similar to the following:

	{
		"environ":"Practice",
		"accountId":"000-000-0000000-000",
		"accessToken":"66d4fa179e078fb023db5e9bc2ff2804-13e9d9726fbe902d1c70114914a5ce6d"
	}

To securely store your AuthInfo.json file on your system, using DPAPI, run:

	OandaTicks.exe <path to AuthInfo.json>

To delete your DPAPI-protected AuthInfo.json file, run:

	OandaTicks.exe /DELETEAUTHINFO

To receive and persists ticks (as one-minute chunks) from the OANDA pricing stream, run:

	OandaTicks.exe

The ticks and setting will be stored in your local app-data path, under a "Louis S. Berman" folder.

	