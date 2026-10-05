---
id: "FR-SAMPLE-001"
type: "requirement"
title: "Sample Feature"
status: "active"
version: "1.0.0"
category: "functional"
derives-from:
  - "UC-SAMPLE-USECASE"
verifiable-by: "cucumber-bdd"
acceptance-format: "gherkin"
cucumber-tags:
  - "@FR-SAMPLE-001"
---

# Requirement: Sample Feature

```gherkin
Feature: Sample Feature
  Scenario: Basic execution
    Given system is ready
    When user triggers action
    Then system responds with 200 OK
```
