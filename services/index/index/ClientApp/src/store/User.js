const fetchUser = 'FETCH_USER';
const removeUser = 'REMOVE_USER';
const initialState = { user: null };

export const actionCreators = {
    fetchUser: _ => async (dispatch) => {
        const response = await fetch(`api/users/validate`);

        const user = await response.json();

        dispatch({ type: fetchUser, user });
    },
    removeUser: () => ({ type: removeUser })
};

export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === fetchUser) {
        return { ...state, user: action.user };
    }

    if (action.type === removeUser) {
        return { ...state, user: null };
    }

    return state;
};
