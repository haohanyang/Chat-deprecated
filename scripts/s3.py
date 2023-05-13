import os
import boto3
import botocore
import sys

# Configs fron environment variables.
ENDPOINT_URL = os.getenv("ENDPOINT_URL")
REGION = os.getenv("REGION")
ACCESS_KEY = os.getenv("ACCESS_KEY")
ACCESS_SECRET = os.getenv("ACCESS_SECRET")
BUCKET_NAME = os.getenv("BUCKET_NAME")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise Exception(
            "Missing arguments. Usage: python3 s3.py <file_key> <file_path>"
        )
    file_key = sys.argv[1]
    file_path = sys.argv[2]

    if (
        (ENDPOINT_URL is None)
        or (REGION is None)
        or (ACCESS_KEY is None)
        or (ACCESS_SECRET is None)
    ):
        raise Exception("Missing environment variables.")

    with open(file_path, "rb") as f:
        data = f.read()

        session = boto3.session.Session()
        client = session.client(
            "s3",
            endpoint_url=ENDPOINT_URL,
            config=botocore.config.Config(s3={"addressing_style": "virtual"}),
            region_name=REGION,
            aws_access_key_id=ACCESS_KEY,
            aws_secret_access_key=ACCESS_SECRET,
        )

        client.put_object(
            Bucket=BUCKET_NAME,
            Key=file_key,
            Body=data,
            ACL="private",
            Metadata={"type": "text-file"},
        )
