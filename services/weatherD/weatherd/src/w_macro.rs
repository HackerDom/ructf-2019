#[macro_export]
macro_rules! log_error {
    ($expression:expr) => (
        println!("{:?} = {:?}",stringify!($expression),$expression); $expression;
    )
}
