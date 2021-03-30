PROBLEM:
As a company that runs HTTP services, questions of scale often come up. When you want to determine how a service will scale
before turning it loose in the wild, it's often prudent to run a load test to simulate your expected traffic. A good load 
generator should be able to provide ample RPS (requests per second) to help drive out potential performance problems before
any of your new users see them - the higher the better.

We would like you to construct a simple HTTP load generator using modern C# async practices. It should accept an input file
specifying details like hostname, path, and requests per second, and then generate the requested load until the program
has been shut down. It should also handle/report on any obviously erroneous behavior from the server.

This task should be timeboxed at somewhere around 2 hours; we are not expecting a world-class application, but merely
would like to get to know you better as a developer through your code. 

DETAILS:
   * Server URL: https://c1i55mxsd6.execute-api.us-west-2.amazonaws.com/Live
   * Permissions (in Header): 'X-Api-Key: RIqhxTAKNGaSw2waOY2CW3LhLny2EpI27i56VA6N'
   * Expected Request Payload (in JSON): { "name": "YOUR_NAME", "date": "NOW_IN_UTC", "requests_sent": REQUESTS_THIS_SESSION }
   * Expected Response Payload (in JSON): { "successful": true }

REQUIREMENTS:
   * Program must accept file-based input for: serverURL, targetRPS, authKey. Additional parameters may be added as desired for
     your clarity and ease of use.
   * Program must send up valid request body payload.
   * Program must sanely handle typical HTTP server responses.
   * Program must output to the console the current RPS and target RPS.
   * After the run has completed, program must output a summary of run including relevant request/response metrics.
   * Your API key is limited to 100,000 requests. Please contact us if you need that limit raised for any reason.

SUBMISSION:
We will review at the end of the interviewing session, and talk through design decisions and tradeoffs.
