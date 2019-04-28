#![allow(dead_code)]
#[allow(unused_imports)]

mod weather_state;
mod create_source_handler;
mod push_message_handler;
mod create_image;

extern crate futures;
extern crate gotham;
#[macro_use]
extern crate gotham_derive;
extern crate hyper;
extern crate mime;
extern crate serde;
#[macro_use]
extern crate serde_derive;
extern crate serde_json;

extern crate tokio_core;

extern crate xml;

use gotham::router::builder::DefineSingleRoute;
use gotham::router::builder::{build_simple_router, DrawRoutes};

pub use self::weather_state::WeatherState;

use self::create_source_handler::CreateSourceHandler;
use self::push_message_handler::PushMessageHandler;
use self::get_list_of_sources_handler::GetListOfSourcesHandler;

mod create_source_query_string_extractor;
mod push_message_query_string_extractor;
mod get_list_of_sources_handler;
mod constants;


use std::sync::{Arc, Mutex};
use serde::de::Unexpected::Str;
use crate::weather_state::Race;
use core::borrow::Borrow;
use ascii::AsciiString;
use ascii::Ascii;
use ascii::AsciiStr;
use std::str::FromStr;

use std::num::Wrapping;
use crate::create_source_query_string_extractor::CreateSourceQueryStringExtractor;
use crate::push_message_query_string_extractor::PushMessageQueryStringExtractor;
use std::path::Path;
use std::io::prelude::*;

use std::env;

use resvg::prelude::*;

use magick_rust::{MagickWand, magick_wand_genesis};
use std::sync::{Once, ONCE_INIT};
use std::fs::File;

extern crate hex;
use openssl::aes::{AesKey, KeyError, aes_ige};
use openssl::symm::Mode;
use hex::{FromHex};

use openssl::symm::{Cipher, Crypter};
use rustc_serialize::hex::ToHex;

mod image_generator;

static START: Once = ONCE_INIT;


use std::io::prelude::*;

mod create_source_dto;

#[macro_use]pub mod w_macro;

fn main()
{
    println!("started");

    START.call_once(|| {
        magick_wand_genesis();
    });

    let weather_state = Arc::new(Mutex::new(WeatherState::new()));

    let router = || {
        return build_simple_router(|route| {
            route
                .post("/create_source")
                .to_new_handler(CreateSourceHandler::new(&weather_state));

            route
                .post("/push_message")
                .to_new_handler(PushMessageHandler::new(&weather_state));

            route
                .get("/get_sources_list")
                .to_new_handler(GetListOfSourcesHandler::new(&weather_state));
        });
    };

    let addr = "0.0.0.0:7878";
    println!("Listening for requests at http://{}", addr);
    gotham::start(addr, router());
}


