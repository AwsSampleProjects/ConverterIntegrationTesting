#!/bin/bash

AWS_REGION="eu-central-1"
INPUT_BUCKET_NAME="converter-input-bucket"
OUTPUT_BUCKET_NAME="converter-output-bucket"
SQS_QUEUE_NAME="converter-queue"

# Create queue
awslocal sqs create-queue --queue-name "$SQS_QUEUE_NAME" --region "$AWS_REGION"

# Initialize queue ARN
QUEUE_ARN=$(awslocal sqs get-queue-attributes \
    --queue-url http://localhost:4566/000000000000/"$SQS_QUEUE_NAME" \
    --attribute-names QueueArn \
    --query 'Attributes.QueueArn' \
    --output text \
    --region "$AWS_REGION")

# Create buckets
awslocal s3 mb s3://"$INPUT_BUCKET_NAME" --region "$AWS_REGION"
awslocal s3 mb s3://"$OUTPUT_BUCKET_NAME" --region "$AWS_REGION"

# Set bucket notification configuration
awslocal s3api put-bucket-notification-configuration \
    --bucket "$INPUT_BUCKET_NAME" \
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
                                "Value": ".json"
                            }
                        ]
                    }
                }
            }
        ]
    }'
    
# Clear queue with test messages
awslocal sqs purge-queue --queue-url http://localhost:4566/000000000000/"$SQS_QUEUE_NAME" --region "$AWS_REGION"