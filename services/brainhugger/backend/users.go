package main

import (
	"brainhugger/backend/storage"
	"brainhugger/backend/cbc"
	"bytes"
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"strconv"
	"strings"
)

type User struct {
	Secret []byte
	Key []byte
}

type UsersManager struct {
	Storage storage.Storage
}

func (um *UsersManager) AddUser(password string, key []byte) (uint, string, error) {
	usersCount := um.Storage.GetItemsCount()

	plainSecret := fmt.Sprintf("%v|%v", usersCount, password)
	encryptedSecret, err := cbc.Encrypt(key, []byte(plainSecret))

	if err != nil {
		return 0, "", errors.New("can not encrypt password: " + err.Error())
	}

	rawUser, err := json.Marshal(User{Secret: encryptedSecret, Key: key})
	if err != nil {
		return 0, "", errors.New("can not marshal user: " + err.Error())
	}
	if err := um.Storage.Set(usersCount, rawUser); err != nil {
		return 0, "", errors.New("can not set storage: " + err.Error())
	}
	um.Storage.IncItemsCount()
	return usersCount, base64.StdEncoding.EncodeToString(encryptedSecret), nil
}

func splitPlainSecret(plainSecret string) (uint, string, error) {
	idx := strings.Index(plainSecret, "|")
	if idx == -1 {
		return 0, "", errors.New("can not find pipe")
	}
	userId, err := strconv.ParseInt(plainSecret[:idx], 10, 64)
	if err != nil {
		return 0, "", errors.New("can not parse userId: " + err.Error())
	}
	return uint(userId), plainSecret[idx + 1:], nil
}

func (um *UsersManager) LoginUser(userId uint, password string) (bool, string, error) {
	if userId >= um.Storage.GetItemsCount() {
		return false, "", nil
	}

	var user User
	rawUser, err := um.Storage.Get(userId)
	if err != nil {
		return false, "", errors.New("can not get user from storage: " + err.Error())
	}
	err = json.Unmarshal(rawUser, &user)
	if err != nil {
		return false, "", errors.New("can not unmarshal user: " + err.Error())
	}
	plainSecret, err := cbc.Decrypt(user.Key, user.Secret)
	if err != nil {
		return false, "", errors.New("can not decrypt password: " + err.Error())
	}
	userId, realPassword, err := splitPlainSecret(string(plainSecret))
	if err != nil {
		return false, "", errors.New("can not split plain secret")
	}
	if string(realPassword) == password {
		return true, base64.StdEncoding.EncodeToString(user.Secret), nil
	} else {
		return false, "", nil
	}
}

func (um *UsersManager) GetForCookie(userId uint) (string, error) {
	var user User
	rawUser, err := um.Storage.Get(userId)
	if err != nil {
		return "", errors.New("can not get user from storage: " + err.Error())
	}
	err = json.Unmarshal(rawUser, &user)
	if err != nil {
		return "", errors.New("can not unmarshal user: " + err.Error())
	}
	return base64.StdEncoding.EncodeToString(user.Secret), nil
}

func (um *UsersManager) GetFromCookie(cookies []*http.Cookie) (uint, []byte, error) {
	var userId uint
	var secretB64 string
	for _, cookie := range cookies {
		if cookie.Name == "uid" {
			uid, err := strconv.ParseInt(cookie.Value, 10, 64)
			if err != nil {
				return 0, nil, errors.New("can not parse user id: " + err.Error())
			}
			userId = uint(uid)
		} else if cookie.Name == "secret" {
			secretB64 = cookie.Value
		}
	}
	secret, err := base64.StdEncoding.DecodeString(secretB64)
	if err != nil {
		return 0, nil, errors.New("can not parse base64 of secretB64: " + err.Error())
	}
	return userId, secret, nil
}

func (um *UsersManager) ValidateCookies(cookies []*http.Cookie) (bool, uint, error) {
	userId, secret, err := um.GetFromCookie(cookies)
	if err != nil {
		return false, 0, errors.New("can not get from cookie: " + err.Error())
	}

	var user User
	rawUser, err := um.Storage.Get(userId)
	err = json.Unmarshal(rawUser, &user)
	if err != nil {
		return false, 0, errors.New("can not unmarshal user: " + err.Error())
	}
	plainSecret, err := cbc.Decrypt(user.Key, secret)
	if err != nil {
		return false, 0, errors.New("can not decrypt password: " + err.Error())
	}
	realUserId, _, err := splitPlainSecret(string(plainSecret))
	if err != nil {
		return false, 0, nil
	}
	if !bytes.Equal(user.Secret, secret) {
		return false, 0, nil
	}
	return realUserId == userId, userId, nil
}

func (um *UsersManager) Init(storagePath string, maxUsersCount uint) {
	um.Storage = storage.Storage{}
	um.Storage.Init(storagePath, maxUsersCount)
}
