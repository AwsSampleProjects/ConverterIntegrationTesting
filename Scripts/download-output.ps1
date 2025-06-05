. "$PSScriptRoot/constants.ps1"

aws s3 sync s3://$OUTPUT_BUCKET_NAME/Contracts ../TestFiles/ContractTest_1/Output/ --endpoint-url $ENDPOINT --region $REGION