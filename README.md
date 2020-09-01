# CRM to Postgres Data Pipeline

This is a lambda function that would:

- Pull data out of CRM (Dynamics365),
- Format the data returned to match the columns of the relevant tables in the contact details database,
- Load the data into the database.

This would be used to migrate data into the postgres DB set up for the [Resident Contact API](https://github.com/LBHackney-IT/resident-contact-api) to retrieve.

## Invoking the Lambda

The lambda can be invoked by going to the function in AWS Lambda and clicking on test.
This would require you to set up a test event, set up an dummy test event and click on the test button to invoke the lambda.

You can also invoke the [lambda with the AWS CLI](https://aws.amazon.com/blogs/architecture/understanding-the-different-ways-to-invoke-lambda-functions/)

If targeting an database with empty tables, the target tables for the added records.
