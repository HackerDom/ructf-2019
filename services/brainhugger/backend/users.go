package main

import (
	"errors"
	"brainhugger/backend/storage"
)

type UsersManager struct {
	Storage storage.Storage
}

func (um *UsersManager) AddUser(password string) (uint, error) {
	usersCount := um.Storage.GetItemsCount()
	if err := um.Storage.Set(usersCount, []byte(password)); err != nil {
		return 0, errors.New("can not add user: " + err.Error())
	}
	um.Storage.IncItemsCount()
	return usersCount, nil
}

func (um *UsersManager) ValidateUserPassword(userId uint, password string) bool {
	realPassword, err := um.Storage.Get(userId)
	if err != nil {
		return false
	}
	return string(realPassword) == password
}

func (um *UsersManager) Init(storagePath string, maxUsersCount uint) {
	um.Storage = storage.Storage{}
	um.Storage.Init(storagePath, maxUsersCount)
}
