#!/bin/bash

AWS_REGION="eu-central-1"
AWS_BUCKET="converter-bucket"
AWS_QUEUE="converter-queue"

# Create queue
awslocal sqs create-queue --queue-name "$AWS_QUEUE" --region "$AWS_REGION"

# Initialize queue ARN
QUEUE_ARN=$(awslocal sqs get-queue-attributes \
    --queue-url http://localhost:4566/000000000000/"$AWS_QUEUE" \
    --attribute-names QueueArn \
    --query 'Attributes.QueueArn' \
    --output text \
    --region "$AWS_REGION")

# Create bucket
awslocal s3 mb s3://"$AWS_BUCKET" --region "$AWS_REGION"

# Set bucket notification configuration
awslocal s3api put-bucket-notification-configuration \
    --bucket "$AWS_BUCKET" \
    --notification-configuration '{
        "QueueConfigurations": [
            {
                "QueueArn": "'$QUEUE_ARN'",
                "Events": ["s3:ObjectCreated:*"],
                "Filter": {
                    "Key": {
                        "FilterRules": [
                            {
                                "Name": "suffix",
                                "Value": ".xml"
                            }
                        ]
                    }
                }
            }
        ]
    }'

# Clear queue with test messages
awslocal sqs purge-queue --queue-url http://localhost:4566/000000000000/"$AWS_QUEUE" --region "$AWS_REGION"