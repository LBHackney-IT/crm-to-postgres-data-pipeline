.PHONY: build
build:
	docker-compose build crm-to-postgres-data-pipeline

.PHONY: shell
shell:
	docker-compose run crm-to-postgres-data-pipeline bash

.PHONY: test
test:
	docker-compose up test-database & docker-compose build crm-to-postgres-data-pipeline-test && docker-compose up crm-to-postgres-data-pipeline-test

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format
