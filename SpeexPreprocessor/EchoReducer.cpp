#include <speex/speex_echo.h>

#include "EchoReducer.h"
#include "SpeexException.h"

using namespace System;
using namespace SpeexEchoReducer;

EchoReducer::EchoReducer(int frameSize, int samplingRate)
{
	_state = speex_echo_state_init(frameSize, samplingRate);
}

EchoReducer::~EchoReducer()
{
	if (_state)
	{
		speex_echo_state_destroy(_state);
		_state = nullptr;
	}
}

void EchoReducer::EchoCapture(array<short>^ input_frame, array<short>^ output_frame)
{
	pin_ptr<short> input_frame_buffer = &input_frame[0];
	pin_ptr<short> output_frame_buffer = &output_frame[0];

	speex_echo_capture(_state, input_frame_buffer, output_frame_buffer);
}

void EchoReducer::EchoPlayback(array<short>^ echo_frame)
{
	pin_ptr<short> echo_frame_buffer = &echo_frame[0];

	speex_echo_playback(_state, echo_frame_buffer);
}

void EchoReducer::EchoCapture(array<unsigned char>^ input_frame, array<unsigned char>^ output_frame)
{
	pin_ptr<unsigned char> input_frame_buffer = &input_frame[0];
	pin_ptr<unsigned char> output_frame_buffer = &output_frame[0];

	speex_echo_capture(_state, (spx_int16_t*)input_frame_buffer, (spx_int16_t*)output_frame_buffer);
}

void EchoReducer::EchoCapture(array<short>^ input_frame, array<unsigned char>^ output_frame)
{
	pin_ptr<short> input_frame_buffer = &input_frame[0];
	pin_ptr<unsigned char> output_frame_buffer = &output_frame[0];

	speex_echo_capture(_state, input_frame_buffer, (spx_int16_t*)output_frame_buffer);
}


void EchoReducer::EchoPlayback(array<unsigned char>^ echo_frame)
{
	pin_ptr<unsigned char> echo_frame_buffer = &echo_frame[0];

	speex_echo_playback(_state, (spx_int16_t*)echo_frame_buffer);
}

void EchoReducer::EchoCancellation(array<unsigned char>^ input_frame, array<unsigned char>^ echo_frame, array<unsigned char>^ output_frame)
{
	pin_ptr<unsigned char> input_frame_buffer = &input_frame[0];
	pin_ptr<unsigned char> echo_frame_buffer = &echo_frame[0];
	pin_ptr<unsigned char> output_frame_buffer = &output_frame[0];

	speex_echo_cancellation(_state, (spx_int16_t*)input_frame_buffer, (spx_int16_t*)echo_frame_buffer, (spx_int16_t*)output_frame_buffer);
}

void EchoReducer::EchoCancellation(array<short>^ input_frame, array<unsigned char>^ echo_frame, array<unsigned char>^ output_frame)
{
	pin_ptr<short> input_frame_buffer = &input_frame[0];
	pin_ptr<unsigned char> echo_frame_buffer = &echo_frame[0];
	pin_ptr<unsigned char> output_frame_buffer = &output_frame[0];

	speex_echo_cancellation(_state, input_frame_buffer, (spx_int16_t*) echo_frame_buffer, (spx_int16_t*)output_frame_buffer);
}


//speex_echo_playback
//speex_echo_capture
