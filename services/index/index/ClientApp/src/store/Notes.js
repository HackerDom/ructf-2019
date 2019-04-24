const startRequest = 'FETCH_NOTES_BEGIN';
const successRequest = 'FETCH_NOTES_SUCCESS';
const endRequest = 'FETCH_NOTES_END';
const initialState = { notes: [], fetching: false };

export const actionCreators = {
    fetchNotes: isPublic => async (dispatch) => {
        dispatch({ type: startRequest });
        await fetch(`api/notes?isPublic=${isPublic}`).then(
            async resp => {
                if (resp.ok)
                    return await resp.json();

                throw resp;
            }).then(notes => dispatch({ type: successRequest, notes }))
            .catch(_ => dispatch({ type: endRequest }));
    }
};

export const reducer = (state, action) => {
    state = state || initialState;

    switch (action.type) {
        case startRequest:
            return { ...state, fetching: true, notes: [] };
        case successRequest:
            return { ...state, fetching: false, notes: action.notes };
        case endRequest:
            return { ...state, fetching: false };

        default:
            return state;
    }
};
