# CRM to Postgres Data Pipeline

This is a lambda function that would:

- Pull data out of CRM (Dynamics365),
- Format the data returned to match the columns of the relevant tables in the contact details database,
- Load the data into the database.

This would be used to migrate data into the postgres DB set up for the [Resident Contact API](https://github.com/LBHackney-IT/resident-contact-api) to retrieve.
