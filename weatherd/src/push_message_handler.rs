#![cfg_attr(feature = "cargo-clippy", allow(clippy::mutex_atomic))]
extern crate futures;
extern crate gotham;
extern crate hyper;
extern crate mime;
extern crate http;
extern crate url;

use std::sync::{Arc, Mutex};

use gotham::error::Result;
use gotham::handler::{Handler, HandlerFuture, NewHandler};
use gotham::state::State;

use gotham::helpers::http::response::*;
use gotham::state::{FromState};

use futures::future;
use futures::future::Future;

use hyper::{StatusCode, Request};
use hyper::Client;

use http::Uri;

use super::weather_state::WeatherState;
use crate::weather_state::WeatherSource;

use crate::create_source_query_string_extractor::PushMessageQueryStringExtractor;
use self::http::request;
use crate::create_image;


#[derive(Clone)]
pub struct PushMessageHandler {
    weather_state: Arc<Mutex<WeatherState>>
}

impl PushMessageHandler {
    pub fn new(state : &Arc<Mutex<WeatherState>>) -> PushMessageHandler {
        PushMessageHandler {
            weather_state: state.clone(),
        }
    }
}

impl Handler for PushMessageHandler {
    fn handle(self, mut state: State) -> Box<HandlerFuture> {

        let query_param = PushMessageQueryStringExtractor::take_from(&mut state);

        let client = Client::new();
        let query_string = format!(
            "source={}&token={}&message={}",
            "source",
            "token",
            "message"
        );

        let uri = format!("http://localhost:5000/sendMessage?{}", query_string).parse::<Uri>().unwrap();
        {

            let mut v = self.weather_state.lock().unwrap();

            let source = v.get_source(&query_param.name);

            if source.password != query_param.password {
                panic!("password mismatch");
            }
        }

        let req = request::Builder::new()
             .method("POST")
             .uri(uri)
             .body(hyper::Body::from(create_image::encode(&query_param.message)))
             .unwrap();

        let result = client
            .request(req)
            .then(|res|
                {
                    match res
                        {
                            Ok(response) => {
                                if response.status() == hyper::StatusCode::OK
                                {
                                    let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "Bingo");
                                    return future::ok((state, response));
                                }
                                let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "Pain");
                                return future::ok((state, response));
                            }
                            Err(e) => {
                                println!("something is wrong {}", e);

                                let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "completely wrong");
                                return future::ok((state, response));
                            }
                        }
                });

        return Box::new(result);
    }
}

impl NewHandler for PushMessageHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}