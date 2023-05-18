import datetime
import time
from typing import List
import flask
from flask_cors import CORS
from dateutil import parser
import requests
import random

RANDOM_API = "https://randomuser.me/api/?results=20"


class UserDto:
    def __init__(self, id: str, username: str, name: str, avatarUrl: str):
        self.id = id
        self.username = username
        self.avatarUrl = avatarUrl
        self.name = name
        self.isOnline = random.choice([True, False])
        print(self.isOnline)

    def toDict(self):
        return {
            "id": self.id,
            "username": self.username,
            "avatarUrl": self.avatarUrl,
            "name": self.name,
            "clientId": "u" + self.id,
            "isOnline": self.isOnline,
        }

    @staticmethod
    def fromDict(d: dict):
        return UserDto(d["id"], d["username"], d["name"], d["avatarUrl"])

    def __eq__(self, __value: object) -> bool:
        if not isinstance(__value, UserDto):
            return False
        return self.id == __value.id


class GroupDto:
    def __init__(
        self, id: int, name: str, avatarUrl: str, owner: UserDto, members: List[UserDto]
    ):
        self.id = id
        self.name = name
        self.avatarUrl = avatarUrl
        self.members = members
        self.owner = owner

    def toDict(self):
        return {
            "id": self.id,
            "name": self.name,
            "avatarUrl": self.avatarUrl,
            "clientId": "g" + str(self.id),
            "members": [member.toDict() for member in self.members],
            "owner": self.owner.toDict(),
        }

    @staticmethod
    def fromDict(d: dict):
        return GroupDto(
            int(d["id"]),
            d["name"],
            d["avatarUrl"],
            UserDto.fromDict(d["owner"]),
            [UserDto.fromDict(member) for member in d["members"]],
        )

    def __eq__(self, __value: object) -> bool:
        if not isinstance(__value, GroupDto):
            return False
        return self.id == __value.id


class UserMessageDto:
    def __init__(
        self,
        id: int,
        sender: UserDto,
        receiver: UserDto,
        content: str,
        time: datetime.datetime = datetime.datetime.now().isoformat(),
    ):
        self.id = id
        self.sender = sender
        self.receiver = receiver
        self.content = content
        self.time = time
        self.wasRead = False
        self.wasDelivered = False

    def toDict(self):
        return {
            "id": self.id,
            "sender": self.sender.toDict(),
            "receiver": self.receiver.toDict(),
            "content": self.content,
            "time": self.time,
            "wasRead": False,
            "wasDelivered": False,
        }

    @staticmethod
    def fromDict(id: int, d: dict):
        return UserMessageDto(
            id,
            UserDto.fromDict(d["sender"]),
            UserDto.fromDict(d["receiver"]),
            d["content"],
            parser.parse(d["time"]).isoformat(),
        )


class GroupMessageDto:
    def __init__(
        self,
        id: str,
        sender: UserDto,
        receiver: GroupDto,
        content: str,
        time: datetime.datetime = datetime.datetime.now().isoformat(),
    ):
        self.id = id
        self.sender = sender
        self.receiver = receiver
        self.content = content
        self.time = time
        self.wasRead = False
        self.wasDelivered = False

    def toDict(self):
        return {
            "id": self.id,
            "sender": self.sender.toDict(),
            "receiver": self.receiver.toDict(),
            "content": self.content,
            "time": self.time,
            "wasRead": False,
            "wasDelivered": False,
        }

    @staticmethod
    def fromDict(id: int, d: dict):
        return GroupMessageDto(
            id,
            UserDto.fromDict(d["sender"]),
            GroupDto.fromDict(d["receiver"]),
            d["content"],
            parser.parse(d["time"]).isoformat(),
        )


me: UserDto = None
users: List[UserDto] = []
groups: List[GroupDto] = []
userMessages: List[UserMessageDto] = []
groupMessages: List[GroupMessageDto] = []

# prepare mock data
response = requests.get(RANDOM_API)
if response.status_code == 200:
    randomUsers: list() = response.json()["results"]
    for randomUser in randomUsers:
        users.append(
            UserDto(
                randomUser["login"]["uuid"],
                randomUser["login"]["username"],
                randomUser["name"]["first"] + " " + randomUser["name"]["last"],
                randomUser["picture"]["large"],
            )
        )
me = users[-1]

owners = random.choices(range(len(users)), k=5)
groupId = 1
for owner in owners:
    members = random.choices(range(len(users)), k=5)
    members.append(owner)
    members.append(len(users) - 1)
    members = set(members)
    groups.append(
        GroupDto(
            groupId,
            users[owner].name + "'s Group",
            users[owner].avatarUrl,
            users[owner],
            [users[member] for member in members],
        )
    )
    groupId += 1

for i in range(len(users) - 1):
    for j in range(100):
        sender = None
        receiver = None
        if j % 2 == 0:
            sender = users[i]
            receiver = me
        else:
            sender = me
            receiver = users[i]
        userMessages.append(UserMessageDto(i + j, sender, receiver, "hello world"))

for i in range(len(groups)):
    group = groups[i]
    for j in range(100):
        sender = group.members[j % len(group.members)]
        groupMessages.append(GroupMessageDto(i + j, sender, group, "hello world"))


def findUser(username: str):
    for user in users:
        if user.username == username:
            return user
    return None


def findGroup(groupId: int):
    for group in groups:
        if group.id == groupId:
            return group
    return None


app = flask.Flask(__name__)
CORS(app)


@app.route("/", methods=["GET"])
def hello():
    return "hello world"


# User controller
@app.route("/api/users/me", methods=["GET"])
def getCurrentUsers():
    return flask.jsonify(me.toDict())


@app.route("/api/users/<username>", methods=["GET"])
def getUser(username: str):
    for user in users:
        if user.username == username:
            return flask.jsonify(user.toDict())
    return "User " + username + " not found", 400


@app.route("/api/users/<username>", methods=["PUT"])
def updateUser(username: str):
    return "not implemented yet"


@app.route("/api/users", methods=["GET"])
def getAllUsers():
    time.sleep(3)
    return flask.jsonify([user.toDict() for user in users])


@app.route("/api/users/<username>/groups", methods=["GET"])
def allJoinedGroups(username: str):
    time.sleep(2)
    joinedGroups = []
    for i in range(len(groups)):
        if username in [user.username for user in groups[i].members]:
            joinedGroups.append(groups[i])
    return flask.jsonify([group.toDict() for group in joinedGroups])


# Group controller
@app.route("/api/groups", methods=["GET"])
def getAllGroups():
    time.sleep(3)
    return flask.jsonify([group.toDict() for group in groups])


@app.route("/api/groups", methods=["POST"])
def createGroup():
    return "not implemented yet"


@app.route("/api/groups/<int:groupId>", methods=["GET"])
def getGroup(groupId: int):
    group = findGroup(groupId)
    if group is None:
        return "Group " + groupId + " not found", 400
    return flask.jsonify(group.toDict())


@app.route("/api/groups/<int:groupId>/members", methods=["POST"])
def addMember(groupId: int):
    group = findGroup(groupId)
    if group is None:
        return "Group " + groupId + " not found", 400
    group.members.append(me)
    return "ok"


@app.route("/api/groups/<int:groupId>/members", methods=["DELETE"])
def removeMember(groupId: int):
    group = findGroup(groupId)
    if group is None:
        return "Group " + groupId + " not found", 400
    group.members.remove(me)
    return "ok"


# chat controller
@app.route("/api/chats/user", methods=["GET"])
def getAllUserChat():
    return flask.jsonify([chat.toDict() for chat in userMessages])


@app.route("/api/chats/group", methods=["GET"])
def getAllGroupChat():
    return flask.jsonify([chat.toDict() for chat in groupMessages])


@app.route("/api/chats/user/<username1>/<username2>", methods=["GET"])
def getUserChat(username1: str, username2: str):
    time.sleep(2)
    chats: List[UserDto] = []
    for userMessage in userMessages:
        if (
            userMessage.sender.username == username1
            and userMessage.receiver.username == username2
        ):
            chats.append(userMessage)
        elif (
            userMessage.sender.username == username2
            and userMessage.receiver.username == username1
        ):
            chats.append(userMessage)
    return flask.jsonify([chat.toDict() for chat in chats])


@app.route("/api/chats/group/<int:groupId>", methods=["GET"])
def getGroupChat(groupId: int):
    time.sleep(2)
    chats: List[GroupDto] = []
    for groupMessage in groupMessages:
        if groupMessage.receiver.id == groupId:
            chats.append(groupMessage)
    return flask.jsonify([chat.toDict() for chat in chats])


@app.route("/api/chats/user/message", methods=["POST"])
def sendUserMessage():
    id = len(userMessages)
    data: UserMessageDto = UserMessageDto.fromDict(id, flask.request.json)
    print(
        "new message from {} to {}".format(data.sender.username, data.receiver.username)
    )
    userMessages.append(data)
    return "ok", 201


@app.route("/api/chats/group/message", methods=["POST"])
def sendGroupMessage():
    id = len(groupMessages)
    data: GroupMessageDto = GroupMessageDto.fromDict(id, flask.request.json)
    print(
        "new message from {} to group {}".format(data.sender.username, data.receiver.id)
    )
    groupMessages.append(data)
    return "ok", 201


if __name__ == "__main__":
    app.run(host="localhost", port=5001, debug=True)
