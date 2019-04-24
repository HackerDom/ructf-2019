package cbc

import (
	"errors"
	"math/rand"
)

const BlockSize = 16
const KeySize = 32
var InitVector = []byte{100, 111, 32, 115, 117, 100, 111, 32, 114, 109, 32, 45, 114, 102, 32, 47}

func getBlockQuad(block []byte, i byte) byte {
	if i % 2 == 0 {
		return block[i / 2] >> 4
	} else {
		return block[i / 2] & 0x0f
	}
}

func setBlockQuad(block []byte, i, value byte) {
	if i % 2 == 0 {
		block[i / 2] |= value << 4
	} else {
		block[i / 2] |= value
	}
}

func encryptBlock(key []byte, block []byte, resultBlock []byte) {
	for i := byte(0); i < KeySize; i++ {
		setBlockQuad(resultBlock, key[i], getBlockQuad(block, i))
	}
}

func decryptBlock(key []byte, block []byte, resultBlock []byte) {
	for i := byte(0); i < KeySize; i++ {
		setBlockQuad(resultBlock, i, getBlockQuad(block, key[i]))
	}
}

func addPadding(block []byte) []byte {
	count := 16 - len(block) % BlockSize
	for i := 0; i < count; i++ {
		block = append(block, byte(count))
	}
	return block
}

func Encrypt(key []byte, block []byte) ([]byte, error) {
	if !IsValidKey(key) {
		return nil, errors.New("invalid key")
	}
	block = addPadding(block)
	result := make([]byte, len(block))
	buffer := make([]byte, BlockSize)
	for i := 0; i < BlockSize; i++ {
		buffer[i] = block[i] ^ InitVector[i]
	}
	encryptBlock(key, buffer, result[:BlockSize])
	for j := 1; j < len(block)/BlockSize; j++ {
		for i := 0; i < BlockSize; i++ {
			buffer[i] = block[BlockSize*j+i] ^ result[BlockSize*(j-1)+i]
		}
		encryptBlock(key, buffer, result[BlockSize*j:BlockSize*(j+1)])
	}
	return result, nil
}

func Decrypt(key []byte, block []byte) ([]byte, error) {
	if len(block) == 0 {
		return nil, errors.New("empty block")
	}
	if len(block) % 16 != 0 {
		return nil, errors.New("invalid block size")
	}
	if !IsValidKey(key) {
		return nil, errors.New("invalid key")
	}
	result := make([]byte, len(block))
	decryptBlock(key, block[:BlockSize], result[:BlockSize])
	for i := 0; i < BlockSize; i++ {
		result[i] ^= InitVector[i]
	}
	for j := 1; j < len(block)/BlockSize; j++ {
		decryptBlock(key, block[BlockSize*j:BlockSize*(j+1)], result[BlockSize*j:BlockSize*(j+1)])
		for i := 0; i < BlockSize; i++ {
			result[BlockSize*j+i] ^= block[BlockSize*(j-1)+i]
		}
	}
	padding := result[len(result) - 1]
	if padding == 0 || padding > 16 {
		return nil, errors.New("invalid padding, out of range [1, ..., 16]")
	}
	for i := len(result) - int(padding); i < len(result); i++ {
		if result[i] != padding {
			return nil, errors.New("invalid padding")
		}
	}
	return result[:len(result) - int(padding)], nil
}

func GenerateKey() []byte {
	key := make([]byte, KeySize)
	for i := 0; i < KeySize; i++ {
		key[i] = byte(i)
	}
	rand.Shuffle(KeySize, func(i, j int) { key[i], key[j] = key[j], key[i] })
	return key
}

func IsValidKey(key []byte) bool {
	if len(key) != KeySize {
		return false
	}
	m := make(map[byte]bool)
	for _, x := range key {
		if x > KeySize {
			return false
		}
		if _, ok := m[x]; ok {
			return false
		} else {
			m[x] = true
		}
	}
	return len(m) == KeySize
}
