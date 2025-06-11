. "$PSScriptRoot/constants.ps1"

aws --endpoint-url $ENDPOINT sqs receive-message `
    --queue-url $QUEUE_URL `
    --region $REGION `
    --max-number-of-messages 10 `
    --wait-time-seconds 0
